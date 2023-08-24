using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using MonitoringService;
using MQTTnet.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

#region CONSTANTS
// RabbitMQ
var rabbitMqExchange = "vehicles_topic";
var rabbitMqVehicleQueue = "sensor.vehicles";
var rabbitMqJamQueue = "monitoring.jam";
var routingKey = "*.sth.#";

// MQTT
var mqttBrokerAddress = "emqx";
var mqttPort = 1883;
// var vehicleTopic = "vehicleSensor/values";
var ekuiperTopic = "ekuiper/jam";
var mqttTopics = new List<string> { ekuiperTopic };

// InfluxDB
var influxdbAddress = "http://influxdb:8086";
var influxdbToken = "jeDCcUAE5JDCVRpAKs97o00XXQikkuOxmITZ9XrOz2soYKBD_wOZ6IfRZidjm78QGdC6LJfaTKBFcNvA7pmsrw==".ToCharArray();
var influxdbOrganization = "vukadin";
var influxdbBucket = "bachelorsThesis";

#endregion 

#region CONNECTION

// RabbitMQ
var factory = new ConnectionFactory { HostName = "rabbitmq" };
var connection = factory.CreateConnection();
var channel = connection.CreateModel();
channel.ExchangeDeclare(rabbitMqExchange, ExchangeType.Topic);
channel.QueueDeclare(queue: rabbitMqVehicleQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
channel.QueueBind(rabbitMqVehicleQueue, rabbitMqExchange, routingKey);

channel.QueueDeclare(rabbitMqJamQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

// MQTT
var mqttService = new MqttService();
await mqttService.ConnectAsync(mqttBrokerAddress, mqttPort);
await mqttService.SubsribeToTopicsAsync(mqttTopics);

// InfluxDB
var influxDbClient = InfluxDBClientFactory.Create(influxdbAddress, influxdbToken);

#endregion

#region LOGIC
// InfluxDB
async Task WriteToInfluxDbAsync(double timestep, string lane, double maxVehicleSpeed, int vehicleCount)
{
    var point = PointData
        .Measurement("traffic_jam")
        .Tag("lane", lane)
        .Field("timestep", timestep)
        .Field("vehicle_count", vehicleCount)
        .Field("max_vehicle_speed", maxVehicleSpeed)
        .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

    await influxDbClient.GetWriteApiAsync().WritePointAsync(point, influxdbBucket, influxdbOrganization);
    Console.WriteLine("Traffic_jam inserted in InlfuxDb.");
}

// MQTT
async Task ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
{
    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload); 
    /*if (e.ApplicationMessage.Topic == vehicleTopic)
    {
        mqttService.PublishMessage("monitoring/vehicles", payload);
        return;
    }*/

    if (e.ApplicationMessage.Topic == ekuiperTopic)
    {
        // sending data to RabbitMq Queue
        channel.BasicPublish(string.Empty, "monitoring.jam", false, null, e.ApplicationMessage.Payload);

        Console.WriteLine($"Too slow traffic: {payload}");
        var jsonData = (JObject)JsonConvert.DeserializeObject(payload);
        double timestep = jsonData.SelectToken("timestep").Value<double>();
        string lane = jsonData.SelectToken("lane").Value<string>();
        double maxVehicleSpeed = jsonData.SelectToken("max_vehicle_speed").Value<double>();
        int vehicleCount = jsonData.SelectToken("vehicle_count").Value<int>();

        await WriteToInfluxDbAsync(timestep, lane, maxVehicleSpeed, vehicleCount);

        return;
    }
    
    Console.WriteLine("Unknown MQTT topic...");
}

mqttService.AddApplicationMessageReceived(ApplicationMessageReceivedAsync);

// RabbitMQ
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ev) =>
{
    var payload = Encoding.UTF8.GetString(ev.Body.ToArray());
    mqttService.PublishMessage("monitoring/vehicles", payload);
};

channel.BasicConsume(queue: rabbitMqVehicleQueue, autoAck: true, consumer: consumer);

while (true) ;
#endregion
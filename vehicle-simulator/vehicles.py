import csv
import json
import time
import pika
import paho.mqtt.client as mqtt

file_path = './traffic.csv'
prev_timestep = -1

mqtt_client_id = 'vehicle_device'
mqtt_rabbitmq_address = 'localhost'
mqtt_port = 1883
mqtt_vehicle_topic = 'vehicleSensor/values'

rabbitmq_vehicle_topic = 'vehicles_topic'
routing_key = 'oneword.sth.unknown.number.of.words'


mqtt_client = mqtt.Client(client_id=mqtt_client_id)
mqtt_client.connect(mqtt_rabbitmq_address, port=1883)

connection = pika.BlockingConnection(pika.ConnectionParameters(mqtt_rabbitmq_address))
channel = connection.channel()
channel.exchange_declare(rabbitmq_vehicle_topic, 'topic')

def format_row(row: dict):
    for key in row:
        if key == 'vehicle_id' or key == 'vehicle_type' or key == 'vehicle_lane':
            continue
        row[key] = float(row[key])
    
    return row

def send_data(vehicle_values):
    for vv in vehicle_values:
        json_payload = json.dumps(vv).encode('utf-8')
        # sending data to MQTT broker
        # mqtt_client.publish(mqtt_vehicle_topic, json_payload)
        # sending data to RabbitMQ broker
        channel.basic_publish(exchange=rabbitmq_vehicle_topic, routing_key=routing_key, body=json_payload)

with open(file_path, 'r') as file:
    csv_reader = csv.DictReader(file, delimiter=';')
    vehicle_values = []
    for row in csv_reader:
        row = format_row(row)
        if row['timestep_time'] != prev_timestep:
            send_data(vehicle_values)
            prev_timestep = row['timestep_time']
            vehicle_values = []
            time.sleep(2)
        vehicle_values.append(row)

connection.close()
mqtt_client.disconnect()
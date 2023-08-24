import csv
import json
import time
import pika
import paho.mqtt.client as mqtt

prev_timestep = -1

mqtt_client = mqtt.Client(client_id='vehicle_device')
mqtt_client.connect('localhost', port=1883)

connection = pika.BlockingConnection(pika.ConnectionParameters('localhost'))
channel = connection.channel()
channel.exchange_declare('test_topic', 'topic')

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
        # mqtt_client.publish('vehicleSensor/values', json_payload)
        # sending data to RabbitMQ broker
        channel.basic_publish(exchange='test_topic', routing_key='oneword.sth.unknown.number.of.words', body=json_payload)

with open('./traffic.csv', 'r') as file:
    csv_reader = csv.DictReader(file, delimiter=';')
    vehicle_values = []
    for row in csv_reader:
        # sending data to MQTT broker
        row = format_row(row)
        if row["timestep_time"] != prev_timestep:
            send_data(vehicle_values)
            prev_timestep = row["timestep_time"]
            vehicle_values = []
            time.sleep(3)
        vehicle_values.append(row)

connection.close()
mqtt_client.disconnect()
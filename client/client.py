import pika
import json
import os

rabbitmq_address = 'localhost'
rabbitmq_jam_queue = 'monitoring.jam'

os.system('cls')
print('timestep\tvehicle_count\tmax_vehicle_speed\tlane')

def callback(ch, method, properties, body):
    string_body = body.decode('utf-8')
    json_body = json.loads(string_body)
    print(f'{json_body["timestep"]}\t\t{json_body["vehicle_count"]}\t\t{json_body["max_vehicle_speed"]}\t\t\t{json_body["lane"]}')

connection = pika.BlockingConnection(pika.ConnectionParameters(rabbitmq_address))
channel = connection.channel()
channel.queue_declare(rabbitmq_jam_queue, False, False, False, False, None)
channel.basic_consume(queue=rabbitmq_jam_queue, on_message_callback=callback, auto_ack=True)

channel.start_consuming()
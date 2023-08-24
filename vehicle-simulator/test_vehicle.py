import paho.mqtt.client as mqtt
import json
import pika

def callback(ch, method, properties, body):
    print(f" [x] {body}")

connection = pika.BlockingConnection(pika.ConnectionParameters('localhost'))
channel = connection.channel()
channel.queue_declare('monitoring.jam', False, False, False, False, None)

channel.basic_consume(queue='monitoring.jam', on_message_callback=callback, auto_ack=True)

channel.start_consuming()
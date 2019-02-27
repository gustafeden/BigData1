/*
 Name:		Feather.ino
 Created:	2/22/2019 12:07:10 PM
 Author:	Gustaf Edén
*/

#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>
#include <WiFiClientSecure.h>
#include <WiFiUdp.h>
#include <ESP8266HTTPClient.h>
#include <AzureIoTHub.h>
#include <AzureIoTProtocol_MQTT.h>
#include <AzureIoTUtility.h>

#include "config.h"
#include "dhtSensor.h"

ESP8266WiFiMulti wifiMulti;
static IOTHUB_CLIENT_LL_HANDLE iotHubClientHandle;
static int interval = DEVICE_SEND_INTERVAL;
unsigned long currentMillis = 0;
unsigned long prevMillis = 0;
static bool messagePending = false;
static bool messageSending = true;

void setup()
{
	pinMode(LED_PIN, OUTPUT);
	digitalWrite(LED_PIN, LOW);
	initSerial();
	initSensor();
	delay(500);
	initMultiWifi();
	initTime();
	iotHubClientHandle = initIotHub();
	delay(1000);
	Serial.print("temp: ");
	Serial.println(readTemperature());
	Serial.print("humidity: ");
	Serial.println(readHumidity());
	digitalWrite(LED_PIN, HIGH);
}

void loop() {
	currentMillis = millis();

	if (!messagePending && messageSending)
	{
		if ((currentMillis - prevMillis) >= interval) {
			prevMillis = currentMillis;

			char messagePayload[MESSAGE_MAX_LEN];
			readMessage(messagePayload);
			sendMessage(iotHubClientHandle, messagePayload);
		}
	}
	IoTHubClient_LL_DoWork(iotHubClientHandle);
	delay(10);
}
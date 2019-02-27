// Physical device information for board and sensor
#define DEVICE_ID             WiFi.macAddress()     // already configured
#define DEVICE_STUDENT        "Gustaf"
#define DEVICE_TYPE           "WeMos TempSensor"
#define DEVICE_LOCATION_LAT   "59.382300"
#define DEVICE_LOCATION_LON   "17.972290"      
//#define DEVICE_SEND_INTERVAL  3600000               //default 3600000 ms = 1 hour
#define DEVICE_SEND_INTERVAL  308571
#define GITHUB_LINK "https://github.com/gustafeden/BigData1"

#define WIFI_SSID             "IoT"
#define WIFI_PASS             "IoT2018!"


//DHT Temperature sensor configuration
#define DHT_PIN               4
#define DHT_TYPE              DHT22


// Pin layout configuration
#define LED_PIN              LED_BUILTIN


// Address configuration, don't remove!
#define MESSAGE_MAX_LEN       256
#define EEPROM_SIZE           512
#define SSID_LEN              32
#define PASS_LEN              32
#define CONNECTIONSTRING_LEN  256
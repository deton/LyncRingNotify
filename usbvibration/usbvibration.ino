// https://github.com/thomasfredericks/Metro-Arduino-Wiring
#include <Metro.h>

const int MOTORPIN = 2;

Metro blink = Metro(400);
bool isVibrationOn = false;
bool isMotorOn = false;
uint32_t now;

void onMotor(void)
{
    digitalWrite(MOTORPIN, HIGH);
    isMotorOn = true;
}

void offMotor(void)
{
    digitalWrite(MOTORPIN, LOW);
    isMotorOn = false;
}

void beginVibration(void)
{
    blink.reset();
    isVibrationOn = true;
}

void endVibration(void)
{
    offMotor();
    isVibrationOn = false;
}

void vibrationLoop(void)
{
    if (!isVibrationOn) {
        return;
    }
    if (blink.check()) {
        if (isMotorOn) {
            offMotor();
        } else {
            onMotor();
        }
    }
}

void parseMessage(char letter)
{
    switch (letter) {
    case 'v':
        beginVibration();
        break;
    case 'V':
        endVibration();
        break;
    default:
        break;
    }
}

void setup()
{
    Serial.begin(115200);
    Mouse.begin();
    pinMode(MOTORPIN, OUTPUT);
}

static void mouseLoop()
{
    // move mouse to avoid screen saver
    static uint32_t mprevms = 0;
    const uint32_t PCLOCKMS = 540000; // 9 [min]
    if (now - mprevms > PCLOCKMS) {
        mprevms = now;
        Mouse.move(1, 0, 0);
        //Serial.write("M");
    }
}

void loop()
{
    now = millis();
    vibrationLoop();
    mouseLoop();
    while (Serial.available()) {
        char letter = Serial.read();
        parseMessage(letter);
    }
}

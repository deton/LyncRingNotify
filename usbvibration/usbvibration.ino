// https://www.pjrc.com/teensy/td_libs_TimerOne.html
#include <TimerThree.h>

const int MOTORPIN = 5;

void parseMessage(int letter)
{
    switch (letter) {
    case 'v':
        Timer3.pwm(MOTORPIN, 512);
        break;
    case 'V':
        Timer3.pwm(MOTORPIN, 0);
        break;
    default:
        break;
    }
}

void setup()
{
    Serial.begin(115200);
    Mouse.begin();
    Timer3.initialize(400000);
}

void loop()
{
    while (Serial.available()) {
        char letter = Serial.read();
        parseMessage(letter);
    }
}

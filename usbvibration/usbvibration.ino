// https://www.pjrc.com/teensy/td_libs_TimerOne.html
#include <TimerThree.h>

const int MOTORPIN = 5; // Timer3 PWM for Pololu A-Star 32U4 Micro (ATmega32U4)
const int SWPIN = 2;
const int32_t DEFAULT_PERIOD = 900000; // 900ms. default timer period

const int SWON = LOW;
const int SWOFF = HIGH;

enum {
    PARSER_DUTY = 1,
    PARSER_PERIOD,
    PARSER_END,
};

#define ACCUMNUM(acc, ch) \
    do { \
        if ((acc) < 0) { \
            (acc) = (ch) - '0'; \
        } else { \
            (acc) = (acc) * 10 + (ch) - '0'; \
        } \
    } while (false)

void parseMessage(int letter)
{
    static uint8_t current_token;
    static int duty = -1;
    static int32_t period = -1;

    switch (letter) {
    case 'v':
        // vibration start with optional duty(0-1023) and period[ms].
        // ex: "v512,500.", "v512.", "v."
        current_token = PARSER_DUTY;
        duty = -1;
        period = -1;
        break;
    case '0':
    case '1':
    case '2':
    case '3':
    case '4':
    case '5':
    case '6':
    case '7':
    case '8':
    case '9':
        switch (current_token) {
        case PARSER_DUTY:
            ACCUMNUM(duty, letter);
            break;
        case PARSER_PERIOD:
            ACCUMNUM(period, letter);
            break;
        default:
            break;
        }
        break;
    case ',':
        if (current_token == PARSER_DUTY) {
            current_token = PARSER_PERIOD;
        }
        break;
    case '.':
        if (period > 0) {
            Timer3.setPeriod(period * 1000);
        }
        if (duty >= 0) {
            Timer3.pwm(MOTORPIN, duty);
        }
        //FALLTHRU
    default:
        current_token = PARSER_END;
        duty = -1;
        period = -1;
        break;
    }
}

void setup()
{
    Serial.begin(115200);
    Timer3.initialize(DEFAULT_PERIOD);
    pinMode(SWPIN, INPUT_PULLUP);
}

void loop()
{
    if (digitalRead(SWPIN) == SWON) {
        Timer3.pwm(MOTORPIN, 0); // vibration off
    }
    while (Serial.available()) {
        int letter = Serial.read();
        parseMessage(letter);
    }
}

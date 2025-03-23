#include <Keyboard.h>

const int rightButtonPin = 2;
const int leftButtonPin = 3;

bool rightWasPressed = false;
bool leftWasPressed = false;

unsigned long debounceDelay = 50; // milliseconds
unsigned long lastRightChangeTime = 0;
unsigned long lastLeftChangeTime = 0;

void setup() {
  pinMode(rightButtonPin, INPUT_PULLUP);
  pinMode(leftButtonPin, INPUT_PULLUP);
  Keyboard.begin();
}

void loop() {
  unsigned long now = millis();

  // --- RIGHT BUTTON ---
  bool rightPressed = digitalRead(rightButtonPin) == LOW;
  if (rightPressed != rightWasPressed && (now - lastRightChangeTime > debounceDelay)) {
    lastRightChangeTime = now;
    if (rightPressed) {
      Keyboard.press(KEY_RIGHT_ARROW);
    } else {
      Keyboard.release(KEY_RIGHT_ARROW);
    }
    rightWasPressed = rightPressed;
  }

  // --- LEFT BUTTON ---
  bool leftPressed = digitalRead(leftButtonPin) == LOW;
  if (leftPressed != leftWasPressed && (now - lastLeftChangeTime > debounceDelay)) {
    lastLeftChangeTime = now;
    if (leftPressed) {
      Keyboard.press(KEY_LEFT_ARROW);
    } else {
      Keyboard.release(KEY_LEFT_ARROW);
    }
    leftWasPressed = leftPressed;
  }

  delay(5); // tiny buffer to reduce CPU churn (optional)
}

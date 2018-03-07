# Coffee-Bot

This project was created for the [Microsoft Bot Marathon](https://ticapacitacion.com/curso/botspt) and its goal is to help the 
user prepare a coffee by turning his coffee maker on, it does this analyzing both texts and images the user can send to him.

### Architecture
The bot was developed in C# using the [Microsoft Bot Framework](https://dev.botframework.com/) and the Microsoft Cognitive 
Services [LUIS](https://www.luis.ai) (Language Understanding) for intents recognition, receiving user input in natural language 
and extracting meaning from it and the [Computer Vision API](https://azure.microsoft.com/en-ca/services/cognitive-services/computer-vision) to analize images.

When LUIS understands the intention as being "the user wants coffee" it makes a POST request to a Node.js API that controls the 
coffee maker, this API is running on a Raspberry Pi connected to a relay board that powers the coffee maker. The Bot was deployed 
in Azure and is able to receive inputs from the services: Slack, Skype and web.

![Fluxograma](https://raw.githubusercontent.com/v-herzog/coffee-bot/master/docs/fluxograma.png)

The project's functions:
* Create a bot capable of understand basic conversation and intents;
* Use the Computer Vision API to analyze and describe images;
* Make an IoT device with Raspberry Pi and a regular home appliance.

### Raspberry Pi

> The Raspberry Pi is a low cost, credit-card sized computer... It’s capable of doing everything you’d expect a desktop computer to do, from browsing the internet and playing high-definition video, to making spreadsheets, word-processing, and playing games.
What’s more, the Raspberry Pi  has the ability to interact with the outside world, and has been used in a wide array of digital maker projects, from music machines and parent detectors to weather stations and tweeting birdhouses with infra-red cameras. Source: [What is a Raspberry Pi](https://www.raspberrypi.org/help/what-%20is-a-raspberry-pi/).


It was used a Raspberry Pi model B with [Raspbian OS](https://www.raspberrypi.org/downloads/raspbian/), running a simple Node.js API connected to a relay board. When the API receives the "coffee maker on" request it sets one of the Pi's GPIO pin to ON state, this pin is connected to the relay coil and opens the connection so that the coffee maker receives power from an outlet. The Node.js API has three routes:

* `cafeteira/ligar`: turns on the coffee maker;
* `cafeteira/desligar`:  turns off the coffee maker;
* `cafeteira/estado`: checks the coffee maker state.

They all return the same JSON information, example if the coffee maker is on:

```
{
  'state': 'ON'
}
```

### Usage

[This site](http://mycoffeebot.azurewebsites.net/) hosts a web and a Skype client so that anyone can talk to the bot, it currently supports ten intentions in Portuguese, this being:
* 'the user greets the bot';
* 'the user wants coffee';
* 'the user wants to know all the bot's possibilities';
* 'the user wants to know what is the bot';
* 'the user wants the bot to describe an image';
* 'the user needs help';
* 'the user says something affirmative';
* 'the user says something negative';
* 'the user wants to turn off the coffee maker';
* 'none of the above'.

Example of conversation using text:

![Exemplo de texto](https://raw.githubusercontent.com/v-herzog/coffee-bot/master/docs/exemplo-texto.gif)

Example of conversation sending images by attachments and by url, if the bot recognizes something that he understands as coffee, he will ask if the user wants a coffee. If the image doesn't contains coffee or a mug, the bot will describe the image and inform the percentange of certant of that description:

![Exemplo de imagem](https://raw.githubusercontent.com/v-herzog/coffee-bot/master/docs/exemplo-upload-imagem.gif)

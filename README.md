# Coffee-Bot

This project was created for the [Microsoft Bot Marathon](https://ticapacitacion.com/curso/botspt) and its goal is to help the 
user prepare a coffee by turning his coffee maker on, it does this analysing both texts and images the user can send to him.

### Architecture
The bot was developed in C# using the [Microsoft Bot Framework](https://dev.botframework.com/) and the Microsoft Cognitive 
Services [LUIS](https://www.luis.ai) (Language Understanding) for intents recognition, receiving user input in natural language 
and extracting meaning from it and the [Computer Vision API](https://azure.microsoft.com/en-ca/services/cognitive-services/computer-vision)
to analize images.

When LUIS understands the intention as being "the user wants coffee" it makes a POST request to a Node.js API that controls the 
coffee maker, this API is running on a Raspberry Pi connected to a relay board that powers the coffee maker. The Bot was deployed 
in Azure and is able to receive inputs from the services: Slack, Skype and web.

![Fluxograma]()

The project's functions:
* Create a bot capable of understand basic conversation and intents;
* Use the Computer Vision API to analyze and describe images;
* Make an IoT device with Raspberry Pi and a regular home appliance.


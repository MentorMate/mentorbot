# MentorBot [![Build status](https://ci.appveyor.com/api/projects/status/0sjergot9u18yu4o/branch/master?svg=true)](https://ci.appveyor.com/project/rosenkolev/mentorbot/branch/master)
[![codecov](https://codecov.io/gh/MentorSource/mentorbot/branch/master/graph/badge.svg)](https://codecov.io/gh/MentorSource/mentorbot)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

What is the idea behind MentorBot? MentorBot is a Google/Hangout Chat bot, that can be taught to perform different tasks.
MentorBot uses machine learning to constantly improve responses and understand more accurately what is needed or what is asked.
MentorBot will constantly improve over time not only by learning itself, but also as new tasks are programmed into it.

#### Tasks & Commands

MentorBot uses commands and tasks execution system that can peform different actions in a distiributes system of nodes.
The tasks can be simple like "Tell me what is the time in Mineapolis" or "What is the hex value of 255".
The tasks can be a complex ones that require integrations with outside systems like "Get my JIRA tasks" or "Pull up timesheet report from OpenAir".
Some sensitive tasks like pulling timesheet report out of OpenAir, cab be executed and verify by a blockchain system because the action deal with sensitive data.

#### Learning Center

MentorBot will have a "Learning Center" portal that mentors can log in and see active messages and/or commands send to the bot.
Mentors can see, how confident the bot feels in what he have been asked to do. Mentors can read through each conversation, mark user questions as utterances of given intent, which will help the bot improve.
Mentors can activate and deactivate different commands and integrations with outside systems.

## Usage

In Google Chat you can ask questions, like:

@MentorBot What is the time?
> Your current time is 16:00.

@MentorBot What is the time in Dallas?
> The time in Dallas is 10:00 AM.

## Build

Just use Visual Studio 2017 or newer.

AppVeyor uses appveyor.yml

## Contribution

### Milestones
* [v1.0 MVP](https://github.com/MentorSource/mentorbot/milestone/1?closed=1)
* [v1.1 OpenAir, AI, Google Calendar](https://github.com/MentorSource/mentorbot/milestone/2?closed=1)
* [v1.2 Wiki, JIRA, Settings, Roles](https://github.com/MentorSource/mentorbot/milestone/3?closed=1)

### Issues & Contributions

If you find a bug or have a feature request, please report them at this repository's issues section. See the [CONTRIBUTING GUIDE](CONTRIBUTING.md) for details on building and contributing to this project.
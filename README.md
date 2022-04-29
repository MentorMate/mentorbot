# MentorBot

[![build](https://github.com/MentorMate/mentorbot/actions/workflows/ci-pipeline.yml/badge.svg)](https://github.com/MentorMate/mentorbot/actions/workflows/ci-pipeline.yml)
[![GitHub Super-Linter](https://github.com/MentorMate/mentorbot/actions/workflows/linter.yml/badge.svg)](https://github.com/MentorMate/mentorbot/actions/workflows/linter.yml)
[![spell check](https://github.com/MentorMate/mentorbot/actions/workflows/spell-check.yml/badge.svg)](https://github.com/MentorMate/mentorbot/actions/workflows/spell-check.yml)
[![codecov](https://codecov.io/gh/MentorMate/mentorbot/branch/main/graph/badge.svg?token=e7x4iMuLTj)](https://codecov.io/gh/MentorMate/mentorbot)
[![dotnet](https://img.shields.io/badge/dotnet-v6.0-blue)](https://dotnet.microsoft.com/download/dotnet/6.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

What is the idea behind MentorBot? MentorBot is a Google/Hangout Chat bot, that can be taught to perform different tasks.
MentorBot uses machine learning to constantly improve responses and understand more accurately what is needed or what is asked.
MentorBot will constantly improve over time not only by learning itself, but also as new tasks are programmed into it.

## Information

### Tasks & Commands

MentorBot uses commands and tasks execution system that can perform different actions in a distribute system of nodes.
The tasks can be simple like "Tell me what is the time in Minneapolis" or "What is the hex value of 255".
The tasks can be a complex ones that require integrations with outside systems like "Get my JIRA tasks" or "Pull up timesheet report from OpenAir".
Some sensitive tasks like pulling timesheet report out of OpenAir, cab be executed and verify by a blockchain system because the action deal with sensitive data.

### Learning Center

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

Just use Visual Studio 2019 or newer.

AppVeyor uses appveyor.yml

## Contribution

### Milestones

* [v1.0 MVP](https://github.com/MentorSource/mentorbot/milestone/1?closed=1)
* [v1.1 OpenAir, AI, Google Calendar](https://github.com/MentorSource/mentorbot/milestone/2?closed=1)
* [v1.2 Wiki, JIRA, Settings, Roles](https://github.com/MentorSource/mentorbot/milestone/3?closed=1)

### Issues & Contributions

If you find a bug or have a feature request, please report them at this repository's issues section. See the [CONTRIBUTING GUIDE](CONTRIBUTING.md) for details on building and contributing to this project.

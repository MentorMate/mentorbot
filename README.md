## MentorBot

What is the idea behind MentorBot? MentorBot is a Google/Hangout Chat bot, that can be teached to perform different tasks.
MentorBot uses machine learning to constantly improve responses and undestand more accuratly what is needed or what is asked.
MentorBot will constantly improve over time not only by teaching itself, but also as new tasks are programed into it.

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

@MentorBot What is the time
> Your current time is 16:00.

@MentorBot what is the time in Dallas
> The time in Dallas is 10:00 AM.

## Build

TODO

## Contribution

### Milestones

#### v1.0 MVP

- Create an chat bot service and register it with Google
- Chat bot can take a single command "What is the current time" and account for different time zones.
- Create the Learning Center portal and display a pie chart of all answered questions agains unanswered.
- The bot need to propose command if it is not sure what is asked.

### v1.1

- Integrate the bot with Google Calendar and OpenAir. Add commands like "Get my meetings", "Get next meeting", etc
- Integrate a basic machine learning algorithm.
- Create a command that send a reminder for missing timesheet in OpenAir.
- Show confident chart in Learning Center.

### v1.2

- Setup distributed nodes with blochain validation and execution
- Extend commands for math and different convertions.

### v1.3

- Include JIRA integration
- Extend "Learning Center"

### Contribute with project management

1. Identify tasks needed for the next milestone.
2. Create milestones.
3. Create issues
	1. Add busness requirements.
	2. Set label feature or bug.
    3. Set label new.
    4. Add task list. For example (design,writing unit tests, develop, verified)
4. Revisit verified stories and add labels closed if accepted. 

### Contribute with design

1. Replace label "new" with "design".
2. Create mockup and/or design.
3. Replace label "design" with "open"

### Contribute with development

1. Download the project from this repository
2. Assign to yourself feature or bug with label "open"
3. Write tests and code
4. Commit to feature branch and create pull request to the branch that is for the current milestone. Example if you have feature: 'Create page footer with text "MentorBot"' and  milestone: 1.0. Create pull request to branch "1.0".
5. Replace label "open" with "resolved"
6. Assosieate the pull request
0. Make reviews to other developers "pull request"

### Contribute with testing

1. Get a feature or bug with label "resolved" and verify it
2. Return it to "open" with a comment or replace the label with "verified"

## Contribute with project management

1. Identify tasks needed for the next milestone.
2. Create milestones.
3. Create issues
	1. Add business requirements.
	2. Set label enhancement or bug.
    3. Set label new.
    4. Add task list. For example (design,writing unit tests, develop, verified)
4. Revisit verified stories and add labels closed if accepted. 

## Contribute with design

1. Replace label "new" with "design".
2. Create MockUp and/or design.
3. Replace label "design" with "open"

## Contribute with development

1. Download the project from this repository
2. Assign to yourself feature or bug with label "open"
3. Write tests and code
4. Commit to feature branch and create pull request to the master branch.
5. Replace label "open" with "resolved"
6. Associate the pull request
0. Make reviews to other developers "pull request"

## Contribute with testing

1. Get a feature or bug with label "resolved" and verify it
2. Return it to "open" with a comment or replace the label with "verified"

## Run in linux docker as production
1. In top folder build the image
   ```
   docker build -t mentorbot-functions -f src\MentorBot.Functions\Dockerfile .
   ```
2. Run
   ```
   docker run -it --rm --name mentorbot-local mentorbot-functions
   ```
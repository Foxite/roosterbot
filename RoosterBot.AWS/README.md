RoosterBot.AWS provides AWS functionality to RoosterBot. This includes:
- An SNS endpoint to NotificationService
- A DynamoDB client to provide user and channel configuration
- A CloudWatch log endpoint

It also provides its AWS credentials to other components via AWSConfigService.
RoosterBot.Schedule provides a way for users to view any schedule from within Discord. It is the original component of this repository, and it in fact predates the concept of Components.

It does not load any schedule data by itself, it depends on another component to do this. This way, this component can serve multiple Discord servers that use schedules that are stored in an entirely different format. There is currently only one such component, but I am planning to add more in the near future.

It is designed to work with school schedules, but with some modification, it could support company schedules.

It uses [CsvHelper](https://joshclose.github.io/CsvHelper/) to read staff member information.
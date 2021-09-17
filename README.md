# RoosterBot

RoosterBot is a foundation for a multi-purpose, multi-platform chat bot written in C#. It uses:
- [Qmmands](https://github.com/Quahu/Qmmands)
- [Newtonsoft.Json](https://www.newtonsoft.com/json)
- [Ben.Demystifier](https://github.com/benaadams/Ben.Demystifier)

It does nothing by itself, but it loads external assemblies known as Components. Refer to the folders in this repository for more documentation on each component.

Technical documentation can be found [here](https://rooster.bot).

## The state of this project
RoosterBot was first written in a time when its design scope was much, much smaller than it is now, and while it has gone through significant evolution, it still has its roots in a time when I had far less experience in coding C# than I do now. While many systems have been reworked several times, and even though it has many features I'm proud of, the amount of technical debt makes it hard to maintain. I've decided to build something new from the ground up, ignoring the current code entirely. This allows me to take full advantage of everything I've learned without being restricted by old code.

As such I'm not going to make any more significant changes to this repository. I'm currently working on a successor to RoosterBot, known simply as RoosterBot4, which will run as a .NET generic host. There is no estimated time for its publication.

However, small-scale bugfixes to RoosterBot will continue as necessary. It will also remain AGPL-3.0 licensed forever.

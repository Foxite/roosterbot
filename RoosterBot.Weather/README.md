This component uses [Weatherbit](https://www.weatherbit.io/) to look up weather predictions from anywhere in the world.
Note that it requires the [cities file](https://www.weatherbit.io/api/meta) to be downloaded separately.

It uses a custom-written API built into the component to access the web API. It uses [Newtonsoft.Json](https://www.newtonsoft.com/json) for this, and [CsvHelper](https://joshclose.github.io/CsvHelper/) to read the aforementioned cities file.

It is unfinished, and development is currently on hold pending separation into a complete [Weatherbit API](https://github.com/Foxite/Weatherbit.NET/).
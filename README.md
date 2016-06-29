# WhoIsTweeting

**WhoIsTweeting** is a small program that monitors your followings and marks them as Online, Away, or Offline.

* Last tweet in 5 minutes &rarr; Online
* Last tweet in 15 minutes &rarr; Away
* Pretty long time since the last tweet &rarr; Offline

## Building

1. Get yourself a copy of Visual Studio 2015 (Community is enough).
2. Clone this repository using this command:

    ```
    git clone https://github.com/Dalgona/WhoIsTweeting.git
    ```

3. Run `git submodule update --init --recursive` to fetch dependencies.
4. Open `WhoIsTweeting.sln` with Visual Studio.
5. Open `MainFrame.cs` with the code editor, and put your own *consumer key* and *consumer secret* at line 50.

    ```cs
    api = new API("YOUR_CONSUMER_KEY_HERE", "YOUR_CONSUMER_SECRET_HERE");
    ```
    
    Consult [dev.twitter.com](https://dev.twitter.com) for information about API keys.

6. Build the solution. :tada:

## License

I have not determined the license for this project yet (possibly MIT).

This project uses [PicoBird Twitter Library](https://github.com/Dalgona/PicoBird), which is licensed under the MIT License.

This project also uses [Newtonsoft Json.NET](https://github.com/JamesNK/Newtonsoft.Json), which is also licensed under the MIT License.

## Copyright

Copyright &copy; 2016. Dalgona <dalgona@hontou.moe>

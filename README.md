Table of Contents
=================

1. [What is KeyUtils?](https://github.com/ipquarx/keyutils#keyutils)
2. [How to use KeyUtils](https://github.com/ipquarx/keyutils#tutorial)
3. [Features List](https://github.com/ipquarx/keyutils#features)
4. [Upcoming Features](https://github.com/ipquarx/keyutils#upcoming)

KeyUtils
========
KeyUtils is a utility to help players recover lost keys from their key.dat files, explore more about how the keydat system works, and in the future, provide them with a safer way to store their key.


Tutorial
========

![](http://i.imgur.com/KjxtDYi.png)

Shown above is a screenshot of the main interface. Here is an explanation of everything:

1. The current decryption mode that's selected. The first (and default) mode allows you to decrypt the keydat from your own blockland installation, assuming you can still play the game normally. In the future, it will also work correctly if you have recently changed your processor and the game no longer works for you. There are also 2 other modes that will be covered in a later section.

2. This is the button you can use to switch between decryption modes.

3. Use this button to select the keydats you want to decrypt. You can select as many as you want (within reason), but they have to be in the same folder.

4. Use this button to select a console.log file. If you want to see why you need to upload a console.log file, click on the button above it, which is...

5. The "What's this?" button. This will explain what you need to do after you've selected your keydats to decrypt. If you don't know what to do, click this.

6. The decrypt button. This will start the decryption process.

7. The tabs bar. Click on any of the tabs to show that tab page.


Features
========

- 3 decryption modes: 
    - The first (and default) mode allows you to decrypt the keydat from your own blockland installation, assuming you can still play the game normally.
    - The second mode allows you to decrypt multiple keydats made on the same computer, if you have a keydat to which you know the key it contains.
    - The third mode allows you to experiment around with keydat decryption. You input a raw MAC address and a processor name (Case sensitive) and it outputs the raw decrypted result in hexadecimal.
- Fake key generator (Just to clarify... They don't work. It's theoretically possible for them to work, but the chances are... 0.00000000000000008%. Sooo... Not that good.)
- Key to BLID converter
- Check for updates automatically!


Upcoming Features
=================

- V2 keydat format! A significantly safer way to store your key.
- Autoupdater!
- Save your processor name and blockland location in a config file to more easily use the program.
- Have a suggestion? Email me! Ipquarx@Gmail.com

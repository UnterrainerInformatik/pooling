[![NuGet](https://img.shields.io/nuget/v/Pooling.svg?maxAge=2592000)](https://www.nuget.org/packages/Pooling/)
 [![license](https://img.shields.io/github/license/unterrainerinformatik/pooling.svg?maxAge=2592000)](http://unlicense.org)  [![Twitter Follow](https://img.shields.io/twitter/follow/throbax.svg?style=social&label=Follow&maxAge=2592000)](https://twitter.com/throbax)  

# General  

This section contains various useful projects that should help your development-process.  

This section of our GIT repositories is free. You may copy, use or rewrite every single one of its contained projects to your hearts content.  
In order to get help with basic GIT commands you may try [the GIT cheat-sheet][coding] on our [homepage][homepage].  

This repository located on our  [homepage][homepage] is private since this is the master- and release-branch. You may clone it, but it will be read-only.  
If you want to contribute to our repository (push, open pull requests), please use the copy on github located here: [the public github repository][github]  

# Pooling  

This class implements a lock-free object pool.  
Such things are very handy when developing games because you wouldn't want to run the garbage collector at all in order to reduce lags. But there are times when you have to use a class instead of a struct and that's the point where you want to hold on to your objects and re-use them.  

This pool does exactly that and it offers you some convenience features like events to hook into or automatic object-creation (using variable constructor signatures) as well.  

Just make the class you'd like to pool implement the PoolItem interface, create a new pool and give it the right constructor fields that are needed to create your object.  
After using the object, just return it to the pool.  

This pool isn't created using a fixed size, but is allowed to grow as big as you need it to be while reusing all objects as much as it can.  
  
#### Example  
    
```csharp
Pool<Sprite> PoolInstance = new Pool<Sprite>(new object[] {spriteBatch, game, tokens.AttackSpriteToken});
```

Then, later on, retrieve a new or reused item from the pool:
```csharp
Sprite s = PoolInstance.Get();

...
s.Update(gameTime);
...
```
This either gives you a reused sprite, or, when needed, automatically creates a new Sprite from scratch.

When you're done with it, return it to the pool:
```csharp
PoolInstance.Return(s);
```

After you're done, clean up the pool:
```csharp
PoolInstance.Dispose();
```
That takes care of your registered events and objects in the pool.

[homepage]: http://www.unterrainer.info
[coding]: http://www.unterrainer.info/Home/Coding
[github]: https://github.com/UnterrainerInformatik/pooling
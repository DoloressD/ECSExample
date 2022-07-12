# ECSExample

This project was pretty interesting as it lead me to try all sorts of different approach to get the desired result. I began with the simple things, removing any unnecessary things. So I removed the colliders as they were not in use. I also turned off V-Sync from the Build Settings. Next step was rewriting the code in my own structure. Now I was with 1000 units running from 40 fps to 60 fps with these small changes. It was still not enough so I've tested other such as addings physics but it only leads to more FPS drops.

After the direct approaches, I ran by Unity Profiler and the FPS were dropping from the fact that there are multiple calls for the Coroutine, no matter how optimised the script was. Since this occured due to the fact that there are thousands of agents, I had to do something outside of the box.

I scrapped everything and tried approaching it with ECS. The reason this was the last resort was because ECS in Unity is unfinished and incomplete. But with ECS, it allows parallel processing which would be perfect for the obstacle at hand. ECS is not beginner-user friendly but I tried my best in making the code as readable, simple and understanble as possible.

The drawback is that while I was working on ECS, I worked on different versions of Unity and with each version, there were certain limitations. Testing costed 3 days of work, I ended up choosing Unity 2019.3.0f6. The latest Unit version, I was able to achieve spawing 25,000 marbles and 10,000 actors with 130FPS sitting stable but the issue was that the AI was not working smartly and I could not implement Hybrid ECS that would work.

With the improved AI and ECS system, I had the FPS starting from 10FPS on at the start but then rise up to 80 and higher FPS with 5000 marbles and 5000 actors. (If this was in a game standpoint, it would have been happening during a loading screen.

Regardless, there was another thing to try; JobSystems. So I rewrote the code for Finding the nearest marble into a Job System, that way it supports multithreaded calls. With that, from the Start it is running at 60FPS with 5000 actors and 5000 marbles and increasing with time.

In Conclusion, there is definitely a lot that can be done and the DOTS system is slowly approaching in Unity. With the way it is currently, it can definitely optimise games to some extent.

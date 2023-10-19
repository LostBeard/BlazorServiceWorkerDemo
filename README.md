# Blazor WASM ServiceWorker Demo

[Live Demo](https://lostbeard.github.io/BlazorServiceWorkerDemo/)

The live demo is nothing special at the moment. In Chrome you acn see teh Blazor messages from the worker handler handling events. Firefox ServiceWorker console can be found at "about:debugging#/runtime/this-firefox"

This code demonstrates loading a Blazor WASM inside a ServiceWorker context and handling any events a ServiceWorker may want to such as fetch.

This is currently a working proof onf concept and likely to change. Any and all feedback is welcome!

This project relies on my other repo [SpawnDev.BlazorJS](https://github.com/LostBeard/SpawnDev.BlazorJS) and more specifically [SpawnDev.BlazorJS.WebWorkers](https://github.com/LostBeard/SpawnDev.BlazorJS#spawndevblazorjswebworkers)


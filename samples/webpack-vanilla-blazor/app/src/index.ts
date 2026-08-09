import './styles.css';
import JSConfetti from 'js-confetti';
import { CounterBridgeHub } from 'typeshim';
// .NET static web assets, importing places them in the webpack output.
import 'favicon.png';
import './sample-data/weather.json';

// NOTE ON AUTO-START: The Blazor boot script does not auto-start if the entry is loaded as a module!
// (See webpack.config.js for the `scriptLoading: 'module'` option.)
import '_framework/blazor.webassembly.js';
await window.Blazor.start(); // not necessary if not module.


const jsConfetti = new JSConfetti();
const hub = CounterBridgeHub.Current;

hub.SetOnCreate((bridge) => {
  bridge.SetOnCountChange((count, x, y) => {
    jsConfetti.addConfettiAtPosition({
      confettiNumber: count,
      confettiDispatchPosition: { x, y },
    });
  });
});

hub.SetOnDispose((bridge) => {
  bridge.instance.dispose();
});

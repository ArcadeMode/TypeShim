import './styles.css';

// This is the blazor entrypoint, importing it is enough to get all dependencies
// NOTE ON AUTO-START: The Blazor boot script does not auto-start if the entry is loaded as a module!
// (See webpack.config.js for the `scriptLoading: 'module'` option.)
import '_framework/blazor.webassembly.js';
// await window.Blazor.start(); // <-- to start Blazor manually (again: if module)


// .NET static web assets, importing places them in the webpack output.
import 'favicon.png';
import './sample-data/weather.json';


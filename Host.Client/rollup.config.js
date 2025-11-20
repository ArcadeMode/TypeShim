import nodeResolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import babel from '@rollup/plugin-babel';
import replace from '@rollup/plugin-replace';
import files from 'rollup-plugin-import-file';
import serve from 'rollup-plugin-serve';
import livereload from 'rollup-plugin-livereload';
import typescript from '@rollup/plugin-typescript';

export default {
   input: 'src/index.tsx',
   output: {
      file: 'public/bundle.js',
      format: 'esm'
   },
    plugins: [
      typescript(),
      files({
        output: 'public',
        extensions: /\.(wasm|dat)$/,
        hash: true,
      }),
      nodeResolve({
         extensions: ['.js', '.jsx', '.ts', '.tsx'],
         dedupe: ['react', 'react-dom']
      }),
      babel({
         babelHelpers: 'bundled',
          presets: [
              ['@babel/preset-react'],
              ['@babel/preset-typescript', { onlyRemoveTypeImports: true }]
          ],
          extensions: ['.js', '.jsx', '.ts', '.tsx'],
         generatorOpts: {
            // Increase the size limit from 500KB to 10MB
            compact: true,
            retainLines: true,
            maxSize: 10000000
         }
      }),
      commonjs(),
      replace({
         preventAssignment: false,
         'process.env.NODE_ENV': '"production"'
      }),
     serve({
       open: true,
       contentBase: 'public',
       port: 3000
     }),
     livereload('public')
   ]
}

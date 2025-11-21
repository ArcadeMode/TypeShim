import React from 'react';

declare class DotnetBootstrapper<TExports> {
    private dotnetObj;
    private exportsPromise;
    Create(): Promise<TExports>;
}
declare function generate(text: string, pixelsPerBlock: number): Promise<string>;
type QrImageProps = {
    text?: string;
    relativePath?: string;
};
declare const QrImage: React.FC<QrImageProps>;

export { DotnetBootstrapper, QrImage, generate };

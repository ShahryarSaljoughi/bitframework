﻿module Bit.Tests.Implementations {
    @ObjectDependency({
        name: "PathProvider"
    })
    export class BitTestPathProvider extends Bit.Implementations.DefaultPathProvider {

        public getProjectsPath(): Array<{ name: string, path: string }> {
            return [{ name: "Bit", path: "bitframework/src/Client/TypeScript" }];
        }

    }
}
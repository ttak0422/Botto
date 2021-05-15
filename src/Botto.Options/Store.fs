namespace Botto.Options

open Fable.Core
open Fable.Core.JS

[<Interface>]
type IDataStore<'Key, 'RawData> =
    abstract Get : 'Key -> Promise<Option<'RawData>>
    abstract Set : 'Key -> 'RawData -> Promise<unit>

[<Interface>]
type IStore<'Key, 'RawData> =
    abstract Store : IDataStore<'Key, 'RawData>

module Store =
    let inline get (env: #IStore<'Key, 'RawData>) (key: 'Key) =
        promise {
            let! raw = env.Store.Get key

            match raw with
            | Some raw -> return (^Data: (static member Decode : 'RawData -> Result< ^Data, ErrMsg >) raw)
            | _ ->
                return
                    Error
                    <| failwith "Not found. The value may not have benn saved."
        }

    let inline set (env: #IStore<'Key, 'RawData>) (key: 'Key) (value: ^Data) =
        promise {
            let raw =
                (^Data: (static member Encode : ^Data -> 'RawData) value)

            do! env.Store.Set key raw
        }

module Helper =

    [<Emit("$0 === undefined")>]
    let isUndefined (x: 'a) : bool = jsNative

module LocalStorage =
    [<Emit("new Promise(resolve => setTimeout(resolve, $0))")>]
    let sleep (ms: int) : Promise<unit> = jsNative

    [<ImportMember("./local-native.js")>]
    let localGet (key: Key) : Promise<JsonString> = jsNative

    [<ImportMember("./local-native.js")>]
    let localSet (key: Key, value: JsonString) : Promise<unit> = jsNative

    type LocalStore private () =

        static member Instance = LocalStore()

        interface IStore<Key, JsonString> with
            member _.Store =
                { new IDataStore<Key, JsonString> with
                    member _.Get key =
                        promise {
                            let! v = localGet key

                            if Helper.isUndefined v then
                                return None
                            else
                                return Some v
                        }

                    member _.Set key value = localSet (key, value) }

    let env = LocalStore.Instance

module ChromeStorage =

    [<ImportMember("./chrome-native.js")>]
    let private chromeGet (key: Key) : Promise<JsonString> = jsNative

    [<ImportMember("./chrome-native.js")>]
    let private chromeSet (key: Key, value: JsonString) : Promise<unit> = jsNative

    type ChromeStore private () =
        static member Instance = ChromeStore()

        interface IStore<Key, JsonString> with
            member _.Store =
                { new IDataStore<Key, JsonString> with
                    member _.Get key =
                        promise {
                            let! v = chromeGet key

                            if Helper.isUndefined v then
                                return None
                            else
                                return Some v
                        }

                    member _.Set key value = chromeSet (key, value) }

    let env = ChromeStore.Instance

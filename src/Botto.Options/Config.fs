namespace Botto.Options

open System
open Elmish
open Fable.Core
open Fable.Core.JS
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Fulma
open Thoth.Json

module Config =

    type Mode =
        | Allow
        | Deny

    type Host =
        { Id: Index
          Host: string }
        static member Default() = { Id = 0; Host = String.Empty }

    type AllowHosts = Hosts of Host list

    type DenyHosts = Hosts of Host list

    type Config =
        { CurrentMode: Mode
          AllowHosts: Host list
          DenyHosts: Host list }
        static member Default() =
            { CurrentMode = Deny
              AllowHosts = []
              DenyHosts = [] }

        static member Encode(config: Config) : JsonString = Encode.toString 0 config

        static member Decode(json: JsonString) : Result<Config, ErrMsg> = Decode.Auto.fromString<Config> json

    type Model =
        { Config: Config
          Form: string
          Editing: Host option
          Initialized: bool }

    type UpdateHosts =
        | Add
        | ApplyEdit of Host
        | Remove of Index

    type MsgErr =
        | Save of exn
        | Load of exn

    type Msg =
        | UpdateForm of string
        | UpdateEditing of Host option
        | SetMode of Mode
        | UpdateHosts of UpdateHosts
        | LoadConfig of Result<Config, ErrMsg>
        | Failure of MsgErr

    let inline save (config: Config) =
#if DEBUG
        Store.set LocalStorage.env CONFIG_KEY config
#else
        Store.set ChromeStorage.env CONFIG_KEY config
#endif

    let inline load () =
#if DEBUG
        Store.get LocalStorage.env CONFIG_KEY
#else
        Store.get ChromeStorage.env CONFIG_KEY
#endif

    let saveConfigCmd (config: Config) =
        Cmd.OfPromise.attempt save config (Save >> Failure)

    let loadConfig () =
        Cmd.OfPromise.either load () LoadConfig (Load >> Failure)

    let init () =
        { Config = getDefault ()
          Form = String.Empty
          Editing = None
          Initialized = false },
        loadConfig ()

    let update (msg: Msg) (model: Model) =
        let cfg = model.Config

        let hosts =
            match model.Config.CurrentMode with
            | Allow -> cfg.AllowHosts
            | Deny -> cfg.DenyHosts

        let addHost host hosts =
            { Id = List.length hosts; Host = host } :: hosts

        let updateHost editing =
            List.map
                (fun host ->
                    if host.Id = editing.Id then
                        editing
                    else
                        host)

        let removeHost index =
            List.filter (fun host -> host.Id <> index)

        let applyCurrentHosts hosts =
            match cfg.CurrentMode with
            | Allow -> { cfg with AllowHosts = hosts }
            | Deny -> { cfg with DenyHosts = hosts }

        match msg with
        | UpdateForm text -> { model with Form = text }, Cmd.none
        | UpdateEditing editing -> { model with Editing = editing }, Cmd.none
        | SetMode mode ->
            let cfg' = { cfg with CurrentMode = mode }
            { model with Config = cfg' }, saveConfigCmd cfg'
        | UpdateHosts msg ->
            match msg with
            | Add ->
                let cfg' =
                    applyCurrentHosts (addHost model.Form hosts)

                { model with
                      Form = String.Empty
                      Config = cfg' },
                saveConfigCmd cfg'
            | ApplyEdit editing ->
                let cfg' =
                    applyCurrentHosts (updateHost editing hosts)

                { model with
                      Editing = None
                      Config = cfg' },
                saveConfigCmd cfg'
            | Remove idx ->
                let cfg' = applyCurrentHosts (removeHost idx hosts)
                { model with Config = cfg' }, saveConfigCmd cfg'
        | LoadConfig result ->
            match result with
            | Ok cfg ->
                { model with
                      Config = cfg
                      Initialized = true },
                Cmd.none
            | Error err ->
                { model with
                      Config = getDefault ()
                      Initialized = true },
                Cmd.ofMsg (err |> failwith |> Load |> Failure)
        | Failure e ->
            match e with
            | Load e ->
                console.log e.Message

                { model with
                      Config = getDefault ()
                      Initialized = true },
                Cmd.none
            | Save e ->
                console.log e.Message
                model, Cmd.none


    let onEnter (msg: Msg) (dispatch: Dispatch<Msg>) =
        function
        | (ev: Browser.Types.KeyboardEvent) when ev.code = ENTER_KEY -> dispatch msg
        | _ -> ()
        |> OnKeyDown

    let modeView (currentMode: Mode) (dispatch: Dispatch<Msg>) =
        let radio name text mode =
            Radio.radio [] [
                Radio.input [ Radio.Input.Name name
                              Radio.Input.Props [ OnChange(fun _ -> SetMode mode |> dispatch)
                                                  Checked(currentMode = mode) ] ]
                str text
            ]

        Panel.panel [] [
            Panel.heading [] [ str "Mode" ]
            Panel.Block.div [] [
                Field.div [] [
                    Control.div [] [
                        radio "mode" (string Deny) Deny
                        radio "mode" (string Allow) Allow
                    ]
                ]
            ]
        ]

    let hostsView (hosts: Host list) (editing: Host option) (form: string) (dispatch: Dispatch<Msg>) =

        let hostInput =
            Panel.Block.div [] [
                Control.div [] [
                    Input.input [ Input.Placeholder "e.g. twitter.com"
                                  Input.ValueOrDefault form
                                  Input.OnChange(fun e -> !!e.target?value |> UpdateForm |> dispatch)
                                  Input.Props [ onEnter (UpdateHosts Add) dispatch ] ]
                ]
            ]

        let normalHostView host =
            Panel.Block.div [ Panel.Block.Props [ OnDoubleClick(fun _ -> UpdateEditing(Some host) |> dispatch) ] ] [
                Control.div [] [
                    str host.Host
                    Delete.delete [ Delete.CustomClass "is-pulled-right"
                                    Delete.OnClick(fun _ -> UpdateHosts(Remove host.Id) |> dispatch) ] []
                ]
            ]

        let editingHostView editing =
            Panel.Block.div [] [
                Control.div [] [
                    Input.input [ Input.ValueOrDefault editing.Host
                                  Input.OnChange
                                      (fun e ->
                                          !!e.target?value
                                          |> fun host ->
                                              Some { editing with Host = host }
                                              |> UpdateEditing
                                              |> dispatch)
                                  Input.Props [ onEnter (UpdateHosts(ApplyEdit editing)) dispatch ] ]
                ]
            ]

        let hostsView editing host =
            match editing with
            | Some editing when editing.Id = host.Id -> editingHostView editing
            | _ -> normalHostView host


        Panel.panel
            []
            (Panel.heading [] [ str "Hosts" ]
             :: hostInput :: List.map (hostsView editing) hosts)

    let loadingView = div [] [ str "loading..." ]

    let view (model: Model) (dispatch: Dispatch<Msg>) : ReactElement =
        div [] [
            if model.Initialized then
                modeView model.Config.CurrentMode dispatch

                match model.Config.CurrentMode with
                | Deny -> hostsView model.Config.DenyHosts model.Editing model.Form dispatch
                | Allow -> hostsView model.Config.AllowHosts model.Editing model.Form dispatch
            else
                loadingView
        ]

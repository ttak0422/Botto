module Botto.Options

open Elmish.Navigation
open Elmish.UrlParser

type Page =
    | Config
    | Document

module Page =
    let root = "#"
    let config = "config"
    let document = "document"

type Page with
    static member ToHash =
        function
        | Config -> Page.root + Page.config
        | Document -> Page.root + Page.document

let parser : Parser<Page -> Page, Page> =
    oneOf [ map Config (s Page.config)
            map Document (s Page.document) ]

open Elmish
open Elmish.React
open Fable.Core
open Fable.React
open Fable.React.Props
open Fulma
open Botto.Options

let navbar =
    let item href elements =
        Navbar.Item.a [ Navbar.Item.Props [ Href href ] ] elements

    let start =
        Navbar.Start.div [] [
            Navbar.Brand.div [] [
                Heading.h1 [ Heading.Props [ Href Page.root ] ] [
                    img [ Src "BOTTO_icon.svg"
                          Alt "logo"
                          Id "botto-brand-logo" ]
                ]
            ]
            item (Page.ToHash Config) [ str <| string Config ]
            item (Page.ToHash Document) [ str <| string Document ]
        ]

    Navbar.navbar [] [
        Container.container [] [ start ]
    ]


type Model =
    { Config: Config.Model
      CurrentPage: Page }

type Msg = ConfigMsg of Config.Msg

let href : Page -> HTMLAttr = Page.ToHash >> Href

let modifyUrl : Page -> Cmd<Msg> = Page.ToHash >> Navigation.modifyUrl

let newUrl : Page -> Cmd<Msg> = Page.ToHash >> Navigation.newUrl

let urlUpdate (result: Page option) (model: Model) : Model * Cmd<Msg> =
    match result with
    | Some page -> { model with CurrentPage = page }, Cmd.none
    | None ->
        JS.console.error "parse failed"
        model, modifyUrl model.CurrentPage

let init (result: Page option) =
    let cfg, cfgCmd = Config.init ()

    let (model, cmd) =
        urlUpdate result { Config = cfg; CurrentPage = Config }

    model,
    Cmd.batch [ Cmd.map ConfigMsg cfgCmd
                cmd ]

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | ConfigMsg msg ->
        let (cfg, cfgCmd) = Config.update msg model.Config
        { model with Config = cfg }, Cmd.map ConfigMsg cfgCmd

let view (model: Model) (dispatch: Dispatch<Msg>) =
    let content =
        function
        | Config -> Config.view model.Config (ConfigMsg >> dispatch)
        | Document -> Document.view

    div [] [
        navbar
        Container.container [ Container.IsFluid ] [
            content model.CurrentPage
        ]
    ]

Program.mkProgram init update view
|> Program.toNavigable (parseHash parser) urlUpdate
|> Program.withReactBatched "botto-options"
|> Program.run

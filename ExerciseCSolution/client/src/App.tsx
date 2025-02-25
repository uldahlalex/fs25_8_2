import {WsClientProvider} from "ws-request-hook";
import {BrowserRouter, Route, Router, Routes} from "react-router";
import Game from "./Game.tsx";
import {Toaster} from "react-hot-toast";
import Admin from "./Admin.tsx";
import Lobby from "./Lobby.tsx";

export default function App() {
    return (<WsClientProvider url={'wss://localhost:8181?id=' + crypto.randomUUID()}>
            <Toaster />
            <Lobby />
            <Game />
            <Admin />
      
        </WsClientProvider>

    )
}
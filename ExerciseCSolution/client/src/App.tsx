import {WsClientProvider} from "ws-request-hook";
import Lobby from "./Lobby.tsx";
import Game from "./Game.tsx";
import {BrowserRouter, Route, Router, Routes} from "react-router";

export default function App() {
    return (<WsClientProvider url={'ws://localhost:8181?ws=' + crypto.randomUUID()}>

            <BrowserRouter>
                <Routes>
                    <Route path="/" element={<Lobby />}/>
                    <Route path="/game/:gameid" element={<Game />}/>
                </Routes>


            </BrowserRouter>
        </WsClientProvider>

    )
}
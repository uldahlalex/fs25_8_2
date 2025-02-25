import {WsClientProvider} from "ws-request-hook";
import {BrowserRouter, Route, Router, Routes} from "react-router";
import Game from "./Game.tsx";

export default function App() {
    return (<WsClientProvider url={'ws://localhost:8181?id=' + crypto.randomUUID()}>

            <BrowserRouter>
                <Routes>
                    <Route path="/" element={<Game />}/>
                </Routes>


            </BrowserRouter>
        </WsClientProvider>

    )
}
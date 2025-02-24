import {WsClientProvider} from "ws-request-hook";
import Lobby from "./Lobby.tsx";
import Game from "./Game.tsx";
import {BrowserRouter, Route, Routes} from "react-router";

export default function App() {
    return (
           <BrowserRouter>
               <Routes>       <WsClientProvider url={'ws://localhost:8080?ws='+crypto.randomUUID()}>

               <Route path="/" element={Lobby()} />
                   <Route path="/game/:gameid" element={Game()} />       </WsClientProvider>

               </Routes>
           </BrowserRouter>
    )
}
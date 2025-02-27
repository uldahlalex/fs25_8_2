import {WsClientProvider} from "ws-request-hook";
import Game from "./Game.tsx";
import {Toaster} from "react-hot-toast";
import Admin from "./Admin.tsx";
import Lobby from "./Lobby.tsx";

export default function App() {
    return (<WsClientProvider url={'ws://localhost:8080?id=' + crypto.randomUUID()}>
            <Toaster />
            <Lobby />
            <Game />
            <Admin />
      
        </WsClientProvider>

    )
}
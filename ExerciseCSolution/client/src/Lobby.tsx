import {useWsClient} from "ws-request-hook";
import {useEffect, useState} from "react";
import {
    ClientEntersLobbyDto,
    MemberHasLeftDto,
    ServerHasClientInLobbyDto,
    StringConstants
} from "./generated-client.ts";
import toast from "react-hot-toast";
import {send} from "vite";

export default function Lobby() {
    
    const {onMessage, sendRequest, send, readyState} = useWsClient();
    const [clients, setClients] = useState<string[]>([''])

    useEffect(() => {
        if (readyState !== 1) return;

        const unsubscribeHandlers = [
            onMessage<ServerHasClientInLobbyDto>(
                StringConstants.ServerHasClientInLobbyDto,
                (dto) => {
                    setClients(dto.allClientIds!)
                }
            ),

            onMessage<MemberHasLeftDto>(
                StringConstants.MemberHasLeftDto,
                (dto) => {
                    setClients(currentClients => currentClients.filter(c => c !== dto.memberId));
                    toast("Client " + dto.memberId + " has left!");
                }
            )
        ];

        const enterLobbyDto: ClientEntersLobbyDto = {
            eventType: StringConstants.ClientEntersLobbyDto,
        };
        send(enterLobbyDto);

        // Cleanup function to unsubscribe all handlers
        return () => {
            unsubscribeHandlers.forEach(unsubscribe => unsubscribe());
        };
    }, [readyState]); // Only depend on readyState

    return (<>
        <div>Clients in the lobbby:</div>
        {
            clients.map(c => <div key={c}>{c}</div>)
        }

    </>)
}
import React, {useEffect, useState} from 'react';
import {BaseDto, useWebSocketWithRequests} from 'ws-request-hook';

interface GameState {
    players: Player[];
    currentQuestion?: Question;
    scores: Record<string, number>;
    phase: 'waiting' | 'question' | 'answer';
    timeLeft: number;
    correctAnswer?: string;
}

interface Player {
    id: string;
    name: string;
    score: number;
}

interface Question {
    id: string;
    question: string;
    options: string[];
}

export interface ServerConfirmsDto extends BaseDto {
    success: boolean;
}

export default function TriviaGame() {
    const [playerName, setPlayerName] = useState('');
    const [roomId, setRoomId] = useState('');
    const [isJoined, setIsJoined] = useState(false);
    const [gameState, setGameState] = useState<GameState>({
        players: [],
        scores: {},
        phase: 'waiting',
        timeLeft: 0
    });

    const ws = useWebSocketWithRequests('ws://localhost:8181');

    useEffect(() => {
        const unsubscribe = ws.onMessage<BaseDto & { gameState: GameState }>('gameStateUpdate',
            (msg) => {
                setGameState(msg.gameState);
            }
        );
        return unsubscribe;
    }, [ws]);

    const joinGame = async () => {
        if (!playerName || !roomId) return;

        try {
            var confirmation = await ws.sendRequest<
                BaseDto & { name: string; topicId: string },
                ServerConfirmsDto
            >({
                eventType: 'clientWantsToSubscribeToTopic',
                name: playerName,
                topicId: roomId
            }, 'serverConfirms');
if(confirmation.success)
            setIsJoined(true);
        } catch (error) {
            console.error('Failed to join game:', error);
        }
    };

    const submitAnswer = async (answer: string) => {
        try {
            await ws.sendRequest<
                BaseDto & { answer: string; roomId: string },
                BaseDto
            >({
                eventType: 'submitAnswer',
                answer,
                roomId
            }, 'answerResponse');
        } catch (error) {
            console.error('Failed to submit answer:', error);
        }
    };

    if (!isJoined) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-purple-600 to-blue-500 flex items-center justify-center p-4">
                <div className="bg-white rounded-lg shadow-xl p-6 w-full max-w-md">
                    <h1 className="text-2xl font-bold text-center mb-6">Join Trivia Game</h1>
                    <div className="space-y-4">
                        <input
                            type="text"
                            placeholder="Enter your name"
                            value={playerName}
                            onChange={(e) => setPlayerName(e.target.value)}
                            className="w-full p-2 border rounded focus:ring-2 focus:ring-purple-500"
                        />
                        <input
                            type="text"
                            placeholder="Enter room ID"
                            value={roomId}
                            onChange={(e) => setRoomId(e.target.value)}
                            className="w-full p-2 border rounded focus:ring-2 focus:ring-purple-500"
                        />
                        <button
                            onClick={joinGame}
                            className="w-full bg-purple-600 text-white py-2 rounded hover:bg-purple-700 transition"
                        >
                            Join Game
                        </button>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gradient-to-br from-purple-600 to-blue-500 p-4">
            <div className="max-w-4xl mx-auto">
                {/* Game Header */}
                <div className="bg-white rounded-lg shadow-xl p-4 mb-4">
                    <div className="flex justify-between items-center">
                        <h2 className="text-xl font-bold">Room: {roomId}</h2>
                        <div className="text-lg font-semibold">
                            Time: {gameState.timeLeft}s
                        </div>
                    </div>
                </div>

                {/* Players List */}
                <div className="bg-white rounded-lg shadow-xl p-4 mb-4">
                    <h3 className="text-lg font-bold mb-2">Players</h3>
                    <div className="grid grid-cols-2 md:grid-cols-3 gap-2">
                        {gameState.players.map((player) => (
                            <div
                                key={player.id}
                                className="bg-purple-100 rounded p-2 flex justify-between"
                            >
                                <span>{player.name}</span>
                                <span className="font-bold">{player.score}</span>
                            </div>
                        ))}
                    </div>
                </div>

                {/* Question Area */}
                {gameState.phase === 'question' && gameState.currentQuestion && (
                    <div className="bg-white rounded-lg shadow-xl p-6">
                        <h3 className="text-xl font-bold mb-4">
                            {gameState.currentQuestion.question}
                        </h3>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            {gameState.currentQuestion.options.map((option) => (
                                <button
                                    key={option}
                                    onClick={() => submitAnswer(option)}
                                    className="bg-purple-600 text-white p-4 rounded-lg hover:bg-purple-700 transition"
                                >
                                    {option}
                                </button>
                            ))}
                        </div>
                    </div>
                )}

                {/* Answer Reveal */}
                {gameState.phase === 'answer' && (
                    <div className="bg-white rounded-lg shadow-xl p-6 text-center">
                        <h3 className="text-xl font-bold mb-2">
                            Correct Answer: {gameState.correctAnswer}
                        </h3>
                        <p className="text-lg">Waiting for next question...</p>
                    </div>
                )}

                {/* Waiting Phase */}
                {gameState.phase === 'waiting' && (
                    <div className="bg-white rounded-lg shadow-xl p-6 text-center">
                        <h3 className="text-xl font-bold">
                            Waiting for more players...
                        </h3>
                        <p className="mt-2">Share room code: {roomId}</p>
                    </div>
                )}
            </div>
        </div>
    );
}
module SecureChat.UI.Api

open Fable.Core
open Fable.Core.JsInterop
open Fetch

type LoginRequest = {
    username: string
    password: string
}

type LoginResponse = {
    token: string
    username: string
}

type Message = {
    id: string
    roomId: string
    username: string
    content: string
    timestamp: int64
    encrypted: bool
}

type Room = {
    id: string
    name: string
}

type ApiError = {
    error: string
    message: string option
}

let private apiBase = ""

let private authHeaders token =
    [
        HttpRequestHeaders.ContentType "application/json"
        HttpRequestHeaders.Authorization $"Bearer {token}"
    ]

let login (request: LoginRequest) =
    promise {
        let! response =
            Fetch.fetch $"{apiBase}/api/login" [
                Method HttpMethod.POST
                Body (toJson request)
                Headers [ HttpRequestHeaders.ContentType "application/json" ]
            ]

        if response.Ok then
            let! data = response.json<LoginResponse>()
            return Ok data
        else
            let! error = response.json<ApiError>()
            return Error error.error
    }

let getMessages (token: string) (roomId: string) =
    promise {
        let! response =
            Fetch.fetch $"{apiBase}/api/messages/{roomId}" [
                Headers (authHeaders token)
            ]

        if response.Ok then
            let! data = response.json<{| messages: Message[] |}>()
            return Ok data.messages
        else
            return Error "Failed to load messages"
    }

let sendMessage (token: string) (roomId: string) (content: string) (encrypted: bool) =
    promise {
        let body = {| roomId = roomId; content = content; encrypt = encrypted |}

        let! response =
            Fetch.fetch $"{apiBase}/api/messages" [
                Method HttpMethod.POST
                Body (toJson body)
                Headers (authHeaders token)
            ]

        return response.Ok
    }

let createRoom (token: string) (name: string) =
    promise {
        let body = {| name = name |}

        let! response =
            Fetch.fetch $"{apiBase}/api/rooms" [
                Method HttpMethod.POST
                Body (toJson body)
                Headers (authHeaders token)
            ]

        if response.Ok then
            let! data = response.json<{| roomId: string; name: string |}>()
            return Ok { id = data.roomId; name = data.name }
        else
            return Error "Failed to create room"
    }
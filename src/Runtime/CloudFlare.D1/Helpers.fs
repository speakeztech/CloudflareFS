module CloudFlare.D1.Helpers

open CloudFlare.D1
open Fable.Core

/// Execute a parameterized query and return all results
let queryAll<'T> (db: D1Database) (query: string) (parameters: obj array) =
    db.prepare(query)
        .bind(parameters)
        .all<'T>()

/// Execute a parameterized query and return first result
let queryFirst<'T> (db: D1Database) (query: string) (parameters: obj array) =
    db.prepare(query)
        .bind(parameters)
        .first<'T>()

/// Execute a statement that modifies data
let execute (db: D1Database) (query: string) (parameters: obj array) =
    db.prepare(query)
        .bind(parameters)
        .run()

/// Create a table if it doesn't exist
let createTable (db: D1Database) (tableName: string) (schema: string) =
    let query = $"CREATE TABLE IF NOT EXISTS {tableName} ({schema})"
    db.exec(query)
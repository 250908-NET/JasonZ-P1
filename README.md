# Project 1

## Overview

An ASP.NET Core MinimalAPI that allows users to pull and draw from a global deck, as well as play blackjack using the deck.
The project will conclude with a presentation of working software to trainers and colleagues.

Due: 2025-09-26

## Endpoints

### Suits

- [X] GET /suits: get all suits (maybe paginate lol)
- [X] GET /suits/{suitId}: get specific suit
- [X] POST /suits: create new suit
- [X] PUT /suits/{suitId}: update existing suit
- [X] PATCH /suits/{suitId}: update existing suit (partial)
- [X] DELETE /suits/{suitId}: delete existing suit

### Cards

- [ ] GET /cards: get all card types (maybe paginate lol)
- [ ] GET /cards/{cardId}: get specific card type
- [ ] POST /cards: create new card type
- [ ] PUT /cards/{cardId}: update existing card type
- [ ] PATCH /cards/{cardId}: update existing card type (partial)
- [ ] DELETE /cards/{cardId}: delete existing card type

### Deck

- [ ] GET /deck: get all cards in global deck (maybe paginate lol)
- [ ] POST /deck: insert card into global deck
- [ ] POST /deck/draw: draw one card randomly from the deck

### Blackjack

- [ ] POST /blackjack: start blackjack game
- [ ] GET /blackjack/{gameId}: return current state of game
- [ ] POST /blackjack/{gameId}/{action}: execute bet/hit/stand 

## Requirements

- [X] Project hosted on GitHub
- [X] README that describes the application and its functionalities
- [X] The application should be ASP.NET Core Minimal API application
- [X] The application should build and run
- [ ] The application should have unit tests and at least 20% coverage
- [X] The application should communicate via HTTP(s) (Must have POST, GET, DELETE)
- [X] Have 1 or more DTOs
- [X] ERD of your models and the relationships between them
- [X] Have at least one many to many (m-m) relationship between the models in your app
- [X] Have 2 or more models
- [X] Persisting data to a SQL Server DB running in a Docker container
- [X] The application should communicate to DB via EF Core (Entity Framework Core)

## Halfway Expectations

- [X] Repo setup
- [X] Minimal API and Xunit project setup and connected (at least one test written)
- [X] Model(s) and Repo(s) layer file structure setup
- [X] At least 1 endpoint tie to a model
- [X] README.md
- [X] ERD

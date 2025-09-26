<template>
  <div class="readme-content">
    <h1>Project 1</h1>

    <h2>Overview</h2>
    <p>
      An ASP.NET Core MinimalAPI that allows users to pull and draw from a global deck, as well
      as play blackjack using the deck. The project will conclude with a presentation of working
      software to trainers and colleagues.
    </p>
    <p>Due: 2025-09-26</p>

    <h2>Endpoints</h2>

    <h3>Suits</h3>
    <ul>
      <li><code>GET /suits</code>: get all suits</li>
      <li><code>GET /suits/{suitId}</code>: get specific suit</li>
      <li><code>POST /suits</code>: create new suit</li>
      <li><code>PUT /suits/{suitId}</code>: update existing suit</li>
      <li><code>PATCH /suits/{suitId}</code>: update existing suit (partial)</li>
      <li><code>DELETE /suits/{suitId}</code>: delete existing suit</li>
    </ul>

    <h3>Cards</h3>
    <ul>
      <li><code>GET /cards</code>: get all card types</li>
      <li><code>GET /cards/{cardId}</code>: get specific card type</li>
      <li><code>POST /cards</code>: create new card type</li>
      <li><code>PUT /cards/{cardId}</code>: update existing card type</li>
      <li><code>PATCH /cards/{cardId}</code>: update existing card type (partial)</li>
      <li><code>DELETE /cards/{cardId}</code>: delete existing card type</li>
    </ul>

    <h3>Deck</h3>
    <ul>
      <li><code>GET /deck</code>: get all cards in global deck</li>
      <li><code>POST /deck</code>: insert card into global deck</li>
      <li><code>POST /deck/draw</code>: draw one card randomly from the deck</li>
      <li>
        <code>POST /deck/draw/{numberOfCards}</code>: draw an arbitrary number of cards randomly
        from the deck
      </li>
    </ul>

    <h3>Blackjack</h3>
    <ul>
      <li><code>POST /blackjack</code>: start blackjack game</li>
      <li><code>GET /blackjack/{gameId}</code>: return current state of game</li>
      <li><code>POST /blackjack/{gameId}/{action}</code>: execute bet/hit/stand</li>
    </ul>

    <h2>Requirements</h2>
    <ul>
      <li>Project hosted on GitHub</li>
      <li>README that describes the application and its functionalities</li>
      <li>The application should be ASP.NET Core Minimal API application</li>
      <li>The application should build and run</li>
      <li>
        The application should have unit tests and at least 20% coverage
        <ul>
          <li>Now with 24% line coverage and 35% branch coverage!</li>
        </ul>
      </li>
      <li>The application should communicate via HTTP(s) (Must have POST, GET, DELETE)</li>
      <li>Have 1 or more DTOs</li>
      <li>ERD of your models and the relationships between them</li>
      <li>Have at least one many to many (m-m) relationship between the models in your app</li>
      <li>Have 2 or more models</li>
      <li>Persisting data to a SQL Server DB running in a Docker container</li>
      <li>The application should communicate to DB via EF Core (Entity Framework Core)</li>
    </ul>

    <h2>Halfway Expectations</h2>
    <ul>
      <li>Repo setup</li>
      <li>Minimal API and Xunit project setup and connected (at least one test written)</li>
      <li>Model(s) and Repo(s) layer file structure setup</li>
      <li>At least 1 endpoint tie to a model</li>
      <li>README.md</li>
      <li>ERD</li>
    </ul>
  </div>
</template>

<style scoped>
.readme-content {
  max-width: 800px;
  margin: 0 auto;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif,
    'Apple Color Emoji', 'Segoe UI Emoji';
  line-height: 1.6;
}

h1,
h2,
h3 {
  border-bottom: 1px solid var(--color-border);
  padding-bottom: 0.3em;
  margin-top: 24px;
  margin-bottom: 16px;
}

h1 {
  font-size: 2em;
}

h2 {
  font-size: 1.5em;
}

h3 {
  font-size: 1.25em;
}

p {
  margin-bottom: 16px;
}

ul {
  padding-left: 2em;
  margin-bottom: 16px;
}

li {
  margin-bottom: 0.25em;
}

code {
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, Courier, monospace;
  background-color: rgba(27, 31, 35, 0.05);
  padding: 0.2em 0.4em;
  margin: 0;
  font-size: 85%;
  border-radius: 3px;
}
</style>

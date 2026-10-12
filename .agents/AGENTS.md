# Organizational Rules

- **Script Placement**: Whenever creating a new script, do not attach it directly to the core player or environment objects. Instead, create a new empty GameObject as a child of the `Managers` object, name it clearly after the script (e.g. "Goal Traverse Manager"), attach the script to that empty object, and link the necessary object references via the Inspector. This keeps the Hierarchy clean and makes it easier for the user to find and configure script settings.

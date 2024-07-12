import createClient from "openapi-fetch";

import { paths } from "./rpgtypes";

const client = createClient<paths>({
  baseUrl: process.env.RPG_API_HOST,
});

export const getPlayerCharacter = async (id: string) => {
  const response = await client.GET("/api/v1/rpg/{system}/{archetype}/{id}", {
    params: {
      path: {
        system: process.env.RPG_SYSTEM ?? "Cyborgs",
        archetype: process.env.RPG_PC_ARCHETYPE ?? "PlayerCharacter",
        id: id,
      },
    },
  });

  return response;
};

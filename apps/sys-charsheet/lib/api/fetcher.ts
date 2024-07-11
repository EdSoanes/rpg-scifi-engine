import createClient from "openapi-fetch";

import { paths } from "./rpgtypes";

const client = createClient<paths>({
  baseUrl: process.env.NEXT_PUBLIC_CRM_SERVICES_API,
});

export const getPlayerCharacter = async (
  system: string,
  archetype: string,
  id: string
) => {
  const response = await client.GET("/api/v1/rpg/{system}/{archetype}/{id}", {
    params: {
      path: {
        system: system,
        archetype: archetype,
        id: id,
      },
    },
  });

  return response;
};

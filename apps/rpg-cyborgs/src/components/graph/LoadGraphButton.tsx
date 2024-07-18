'use client'

import { useAtom } from 'jotai'
import { graphFetchAtom } from '../../lib/rpg-api/fetcher'
import React from 'react'
import { Button } from '@chakra-ui/react'

function LoadGraphButton() {
  const [, fetchGraphState] = useAtom(graphFetchAtom)

  return (
    <div>
      <Button colorScheme="teal" variant="outline" onClick={fetchGraphState}>
        Get Benny
      </Button>
    </div>
  )
}

export default LoadGraphButton

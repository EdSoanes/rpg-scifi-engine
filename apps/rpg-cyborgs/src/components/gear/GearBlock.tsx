import { Heading, Stack, StackItem } from '@chakra-ui/react'
import React from 'react'
import { RpgContainer, RpgEntity } from '../../lib/rpg-api/types'
import GearPanel from './GearPanel'

export interface GearBlockProps {
  name: string
  container?: RpgContainer
}

function GearBlock(props: GearBlockProps) {
  const { name, container } = props
  return (
    <Stack w={'100%'}>
      <StackItem>
        <Heading as="h3" size="lg" paddingBottom={4} paddingTop={10}>
          {name}
        </Heading>
      </StackItem>
      <StackItem>
        <Stack
          direction={'row'}
          w={'100%'}
          wrap={'wrap'}
          alignItems={'stretch'}
        >
          {container &&
            container.contents.map((item, i) => (
              <GearPanel key={i} item={item as RpgEntity} />
            ))}
        </Stack>
      </StackItem>
    </Stack>
  )
}

export default GearBlock

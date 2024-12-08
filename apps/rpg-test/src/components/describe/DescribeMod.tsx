'use client'

import ReactJson from 'react-json-view'
import { Dice, ModDescription } from '@lib/rpg-api/types'
import { For, Heading, HStack, Stack, Text } from '@chakra-ui/react'
import { diceValue } from './utils'

export interface DescribeModProps {
  description?: ModDescription | null
  level: number
}

export default function DescribeMod(props: DescribeModProps) {
  const { description, level } = props

  const sourceName = description?.sourceProp
    ? `${description.sourceProp.entityName}.${description.sourceProp.prop}`
    : `${description?.modType}`

  const sourceValue = description?.sourceProp
    ? diceValue(description.sourceProp.value as Dice)
    : diceValue(description?.sourceValue as Dice)

  const padLeft = `${level * 10}px`

  return (
    description && (
      <Stack>
        <HStack alignItems={'center'} paddingLeft={padLeft}>
          <Heading size={'sm'}>{sourceName}</Heading>
          <Text textStyle={'lg'}>{sourceValue}</Text>
        </HStack>
        {description.sourceProp?.mods?.length && (
          <For each={description.sourceProp.mods}>
            {(d) => <DescribeMod description={d} level={level + 1} />}
          </For>
        )}
        {/* <ReactJson src={description} collapsed={false} /> */}
      </Stack>
    )
  )
}

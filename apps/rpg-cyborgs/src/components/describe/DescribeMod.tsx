'use client'

import { Dice, ModInfo } from '@lib/rpg-api/types'
import { For, Heading, HStack, Stack, Text } from '@chakra-ui/react'
import { diceValue } from './utils'

export interface DescribeModProps {
  description?: ModInfo | null
  level: number
}

export default function DescribeMod(props: DescribeModProps) {
  const { description, level } = props

  const sourceName = description?.source
    ? `${description.source.name}.${description.source.prop}`
    : `${description?.modType}`

  const sourceValue = diceValue(
    description?.value ?? (description?.source?.value as Dice)
  )

  const padLeft = `${level * 10}px`

  return (
    description && (
      <Stack>
        <HStack alignItems={'center'} paddingLeft={padLeft}>
          <Heading size={'sm'}>{sourceName}</Heading>
          <Text textStyle={'lg'}>{sourceValue}</Text>
        </HStack>
        {description.source?.mods?.length && (
          <For each={description.source.mods}>
            {(d) => <DescribeMod description={d} level={level + 1} />}
          </For>
        )}
        {/* <ReactJson src={description} collapsed={false} /> */}
      </Stack>
    )
  )
}

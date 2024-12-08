'use client'

import ReactJson from 'react-json-view'
import { Dice, PropDescription } from '@lib/rpg-api/types'
import { For, Stack } from '@chakra-ui/react'
import DescribeProp from './DescribeProp'
import DescribeMod from './DescribeMod'

export interface DescribePropertyProps {
  description?: PropDescription | null
}

export default function DescribeProperty(props: DescribePropertyProps) {
  const { description } = props

  return (
    description && (
      <Stack>
        <DescribeProp
          name={description.rootProp}
          dice={description?.value as Dice}
          level={0}
        />
        <For each={description.mods}>
          {(description) => <DescribeMod description={description} level={1} />}
        </For>
        <ReactJson src={description} collapsed={false} />
      </Stack>
    )
  )
}

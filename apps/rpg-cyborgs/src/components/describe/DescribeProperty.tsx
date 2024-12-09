'use client'

import ReactJson from 'react-json-view'
import { Dice, ObjectPropInfo, ThresholdMod } from '@lib/rpg-api/types'
import { For, Stack } from '@chakra-ui/react'
import DescribeProp from './DescribeProp'
import DescribeMod from './DescribeMod'

export interface DescribePropertyProps {
  description?: ObjectPropInfo | null
}

export default function DescribeProperty(props: DescribePropertyProps) {
  const { description } = props

  const thresholdMod = description?.propInfo.mods.find(
    (x) => x.modType === 'Threshold'
  )
  const baseMods =
    description?.propInfo.mods.filter(
      (x) =>
        x.modType === 'Base' ||
        x.modType === 'Initial' ||
        x.modType === 'Override'
    ) ?? []

  const mods =
    description?.propInfo.mods.filter(
      (x) =>
        x.modType !== 'Threshold' &&
        x.modType !== 'Base' &&
        x.modType !== 'Initial' &&
        x.modType !== 'Override'
    ) ?? []

  return (
    description && (
      <Stack>
        <DescribeProp
          name={description.propPath}
          dice={description.propInfo.value as Dice}
          level={0}
        />
        <For each={mods}>
          {(description) => <DescribeMod description={description} level={1} />}
        </For>
        <ReactJson src={description} collapsed={false} />
      </Stack>
    )
  )
}

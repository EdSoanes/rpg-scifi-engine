import React from 'react'
import { Text, Grid, Progress, GridItem } from '@chakra-ui/react'
import { StatPanel } from '../stats'

export declare interface PointPanelProps {
  name: string
  max: number
  current: number
}

function PointPanel(props: PointPanelProps) {
  const color =
    props.current < props.max / 3
      ? 'red'
      : props.current < (props.max / 3) * 2
        ? 'yellow'
        : 'green'

  return (
    <Grid width="100%" gap={4} templateColumns={'repeat(6, 1fr)'}>
      <GridItem colSpan={1}>{props.name}</GridItem>
      <GridItem colSpan={4}>
        <Progress
          w={'100%'}
          max={props.max}
          colorScheme={color}
          size="md"
          value={props.current}
        />
      </GridItem>
      <GridItem colSpan={1}>Max {props.max}</GridItem>
    </Grid>
  )
}

export default PointPanel

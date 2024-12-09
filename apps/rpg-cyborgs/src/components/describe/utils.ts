import { Dice } from '@lib/rpg-api/types'

export function diceValue(dice?: Dice | null): string | undefined {
  const value = dice?.isConstant
    ? Number(dice?.expr) > 0
      ? `+${dice?.expr}`
      : dice.expr
    : dice?.expr

  return value
}

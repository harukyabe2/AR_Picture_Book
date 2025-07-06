## VSCodeでの操作方法について  <!--#を1〜6個置くとサイズが大から小の見出し-->
# 作業開始前  
git switch main (mainブランチに移動する)  
git pull origin main (mainブランチの最新状態を取得)  
git switch -c 自分のブランチ名 (自分のブランチに移動する *ただし、-cは作成時のみ)  
git merge main (mainブランチの内容をマージ)
# 作業後  
git add .  
git commit -m "変更内容"  
git push origin 自分のブランチ名  
# push後
Github上のPull requestsでPull Requestを作成  
レビューとmerge (mainブランチに取り込む)  
<!--半角スペース2つで改行-->
# 注意点
mainブランチに直接pushはしない  
必ずmainをpullしてから作業を開始する  
